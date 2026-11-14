<!-- Copyright (C) 2023-2024  Kai Lawrence -->
<!--
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
-->

<!--
    Zähler als aufklappbare Tabelle: aufgeklappt die Zählerstände (beim ersten Aufklappen
    nachgeladen), neuester zuerst, mit Verbrauch seit dem vorigen Stand.
-->
<script lang="ts">
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
    import {
        InlineLoading,
        InlineNotification
    } from 'carbon-components-svelte';

    import { WalterDataTable, WalterZaehler } from '$walter/components';
    import WalterDataWrapperQuickAdd from '../elements/WalterDataWrapperQuickAdd.svelte';
    import WalterItemList from '../elements/WalterItemList.svelte';
    import { fileURL } from '$walter/services/files';
    import WalterZaehlerstand from '../details/WalterZaehlerstand.svelte';
    import { convertDateCanadian } from '$walter/services/utils';
    import {
        WalterZaehlerEntry,
        WalterZaehlerstandEntry,
        validateZaehler,
        zaehlerstandItems
    } from '$walter/lib';

    const headers = [
        { key: 'kennnummer', value: 'Kennnummer' },
        { key: 'adresse.anschrift', value: 'Adresse' },
        { key: 'wohnung.text', value: 'Wohnung' },
        { key: 'typ.text', value: 'Typ' },
        { key: 'lastZaehlerstand.datum', value: 'Letztes Ablesedatum' },
        { key: 'lastZaehlerstand.stand', value: 'Letzter Stand' },
        { key: 'lastZaehlerstand.einheit', value: 'Einheit' },
        { key: 'ende', value: 'Ende' },
        { key: 'button', value: '' }
    ];

    export let rows: WalterZaehlerEntry[] | undefined = undefined;
    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let entry: Partial<WalterZaehlerEntry> | undefined = {};
    export let fetchImpl: typeof fetch;

    export let ablesedatum: Date = new Date();
    let quickAddEntry: Partial<WalterZaehlerstandEntry> = {};

    function add(e: Event, zaehler: WalterZaehlerEntry) {
        e.stopPropagation();
        quickAddEntry = {
            datum: convertDateCanadian(ablesedatum),
            stand: zaehler.lastZaehlerstand?.stand,
            einheit: zaehler.lastZaehlerstand?.einheit,
            zaehler: { id: `${zaehler.id}`, text: zaehler.kennnummer }
        };

        open = true;
    }

    $: transformRow = (row: DataTableRow) => {
        const zaehler = row as unknown as WalterZaehlerEntry;
        return {
            ...zaehler,
            button:
                zaehler.lastZaehlerstand?.datum ===
                convertDateCanadian(ablesedatum)
                    ? 'disabled'
                    : {
                          onClick: (e: MouseEvent) => add(e, zaehler),
                          iconDescription: 'Zählerstand hinzufügen'
                      }
        };
    };

    $: rowsAdd = rows?.map(transformRow);

    let open = false;

    const fetchData =
        rows === undefined
            ? (p: Parameters<typeof WalterZaehlerEntry.GetPaged>[1]) =>
                  WalterZaehlerEntry.GetPaged<WalterZaehlerEntry>(fetchImpl, p)
            : undefined;

    // Der ganze Zähler (Stände, Notiz) je Zeile, beim ersten Aufklappen geladen.
    let expandedRowIds: (string | number)[] = [];
    type Geladen = WalterZaehlerEntry | 'laedt' | { fehler: string };
    let geladen: Record<string, Geladen> = {};
    const setze = (id: string | number, wert: Geladen) =>
        (geladen = { ...geladen, [id]: wert });

    async function ladeZaehler(id: string | number) {
        setze(id, 'laedt');
        try {
            const zaehler = await WalterZaehlerEntry.GetOne<WalterZaehlerEntry>(
                `${id}`,
                fetchImpl
            );
            setze(id, zaehler);
        } catch (e) {
            setze(id, { fehler: e instanceof Error ? e.message : `${e}` });
        }
    }

    $: for (const id of expandedRowIds) {
        if (!(id in geladen)) void ladeZaehler(id);
    }

    /** Nach einem neuen Stand: neu laden, falls aufgeklappt, sonst beim nächsten Aufklappen. */
    function standHinzugefuegt() {
        const id = quickAddEntry.zaehler?.id;
        if (id === undefined) return;
        const { [id]: _alt, ...rest } = geladen;
        geladen = rest;
    }

    $: submitDisabled = !validateZaehler(entry);
</script>

<WalterDataWrapperQuickAdd
    title={quickAddEntry.zaehler?.text || 'Zähler'}
    addEntry={quickAddEntry}
    addUrl={WalterZaehlerstandEntry.ApiURL}
    bind:addModalOpen={open}
    onSubmit={standHinzugefuegt}
>
    <WalterZaehlerstand {fetchImpl} entry={quickAddEntry} />
</WalterDataWrapperQuickAdd>

<WalterDataTable
    addUrl={WalterZaehlerEntry.ApiURL}
    addEntry={entry}
    {submitDisabled}
    layout={title !== undefined ? 'accordion' : 'inline'}
    accordionTitle={title}
    quickAddTitle={title}
    expandable
    bind:expandedRowIds
    rows={rowsAdd}
    {fetchData}
    {transformRow}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterZaehler {fetchImpl} bind:entry />
    {/if}
    <svelte:fragment slot="expanded-row" let:row>
        {@const zaehler = geladen[row.id]}
        {#if zaehler === undefined || zaehler === 'laedt'}
            <InlineLoading description="Lade Zählerstände…" />
        {:else if 'fehler' in zaehler}
            <InlineNotification
                kind="error"
                lowContrast
                hideCloseButton
                title="Zählerstände nicht verfügbar"
                subtitle={zaehler.fehler}
            />
        {:else}
            <WalterItemList
                items={zaehlerstandItems(zaehler.staende ?? [])}
                leerText="Noch kein Zählerstand."
                moreHref={`/zaehler/${row.id}`}
                moreText="Zähler öffnen →"
                notiz={zaehler.notiz}
                attachmentsUrl={fileURL.zaehler(`${row.id}`)}
                {fetchImpl}
            />
        {/if}
    </svelte:fragment>
</WalterDataTable>
