<!-- Copyright (C) 2023-2026  Kai Lawrence -->
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

<script lang="ts">
    import {
        Accordion,
        AccordionItem,
        Button,
        DataTable,
        InlineNotification,
        Row,
        Column,
        Tile,
        Toolbar,
        ToolbarContent
    } from 'carbon-components-svelte';
    import { Add, TrashCan } from 'carbon-icons-svelte';
    import { onMount } from 'svelte';
    import { WalterComboBox, WalterNumberInput, WalterDatePicker } from '$walter/components';
    import { walter_selection, walter_get, walter_post, walter_delete } from '$walter/services/requests';
    import { convertEuro } from '$walter/services/utils';
    import type { WalterSelectionEntry } from '$walter/lib';

    export let fetchImpl: typeof fetch;
    export let vertragId: number | undefined = undefined;
    export let umlageId: number | undefined = undefined;
    export let title = 'NK-Anteile';
    export let readonly = false;

    type NkAnteilRow = {
        id: string;
        buchungssatzId: string;
        betrag: number;
        datum: string;
        betreffendesJahr: number;
        notiz: string;
        vertragText: string;
        umlageText: string;
        canRemove: boolean;
    };

    let rows: NkAnteilRow[] = [];
    let loading = true;
    let error = '';

    let newUmlage: WalterSelectionEntry | undefined = undefined;
    let newVertrag: WalterSelectionEntry | undefined = undefined;
    let newBetrag = 0;
    let newJahr = new Date().getFullYear() - 1;
    let newDatum = new Date().toISOString().slice(0, 10);
    let submitting = false;
    let submitError = '';

    const umlagen = walter_selection.umlagen(fetchImpl);
    const vertraege = walter_selection.vertraege(fetchImpl);

    $: canSubmit =
        !readonly &&
        !submitting &&
        newBetrag > 0 &&
        (umlageId != null || newUmlage?.id != null) &&
        (vertragId != null || newVertrag?.id != null);

    async function loadRows() {
        loading = true;
        error = '';
        try {
            const qs = new URLSearchParams();
            if (vertragId != null) qs.set('vertragId', `${vertragId}`);
            if (umlageId != null) qs.set('umlageId', `${umlageId}`);
            const url = qs.size
                ? `/api/vertrags-nk-anteile?${qs}`
                : '/api/vertrags-nk-anteile';
            const data = (await walter_get(url, fetchImpl)) as {
                id: string;
                betrag: number;
                datum: string;
                betreffendesJahr: number;
                notiz?: string;
                vertrag: WalterSelectionEntry;
                umlage: WalterSelectionEntry;
                permissions: { remove: boolean };
            }[];
            rows = data.map((d, i) => ({
                id: String(i),
                buchungssatzId: d.id,
                betrag: d.betrag,
                datum: d.datum,
                betreffendesJahr: d.betreffendesJahr,
                notiz: d.notiz ?? '',
                vertragText: d.vertrag?.text ?? '',
                umlageText: d.umlage?.text ?? '',
                canRemove: d.permissions?.remove ?? false
            }));
        } catch (e) {
            error = String(e);
        } finally {
            loading = false;
        }
    }

    onMount(loadRows);

    async function submit() {
        submitting = true;
        submitError = '';
        try {
            await walter_post('/api/vertrags-nk-anteile', {
                betrag: newBetrag,
                datum: newDatum,
                betreffendesJahr: newJahr,
                umlage: newUmlage ?? { id: umlageId, text: '' },
                vertrag: newVertrag ?? { id: vertragId, text: '' }
            });
            newBetrag = 0;
            await loadRows();
        } catch (e) {
            submitError = String(e);
        } finally {
            submitting = false;
        }
    }

    async function remove(buchungssatzId: string) {
        try {
            await walter_delete(`/api/vertrags-nk-anteile/${buchungssatzId}`);
            await loadRows();
        } catch (e) {
            error = String(e);
        }
    }

    const headers = [
        ...(umlageId == null ? [{ key: 'umlageText', value: 'Umlage' }] : []),
        ...(vertragId == null ? [{ key: 'vertragText', value: 'Vertrag' }] : []),
        { key: 'betreffendesJahr', value: 'Jahr' },
        { key: 'betrag', value: 'Betrag (€)' },
        { key: 'datum', value: 'Datum' },
        { key: 'actions', value: '' }
    ];
</script>

<Accordion>
    <AccordionItem>
        <svelte:fragment slot="title">
            {title} ({rows.length})
        </svelte:fragment>

        <Tile style="overflow: auto;">
            {#if error}
                <InlineNotification
                    kind="error"
                    title="Fehler:"
                    subtitle={error}
                    hideCloseButton
                />
            {/if}

            {#if rows.length === 0 && !loading}
                <p style="margin-bottom: 1rem;">Keine NK-Anteile vorhanden.</p>
            {:else}
                <DataTable
                    size="short"
                    {headers}
                    {rows}
                    style="margin-bottom: 1rem;"
                >
                    <Toolbar><ToolbarContent /></Toolbar>
                    <svelte:fragment slot="cell" let:row let:cell>
                        {#if cell.key === 'betrag'}
                            {convertEuro(cell.value)}
                        {:else if cell.key === 'actions'}
                            {#if row.canRemove && !readonly}
                                <Button
                                    kind="ghost"
                                    size="small"
                                    icon={TrashCan}
                                    iconDescription="Löschen"
                                    on:click={() => remove(row.buchungssatzId)}
                                />
                            {/if}
                        {:else}
                            {cell.value}
                        {/if}
                    </svelte:fragment>
                </DataTable>
            {/if}

            {#if !readonly}
                <Row style="margin-top: 1rem;">
                    {#if umlageId == null}
                        <Column>
                            <WalterComboBox
                                titleText="Umlage"
                                entries={umlagen}
                                bind:value={newUmlage}
                                on:select={(e) => { newUmlage = e.detail?.selectedItem; }}
                            />
                        </Column>
                    {/if}
                    {#if vertragId == null}
                        <Column>
                            <WalterComboBox
                                titleText="Vertrag"
                                entries={vertraege}
                                bind:value={newVertrag}
                                on:select={(e) => { newVertrag = e.detail?.selectedItem; }}
                            />
                        </Column>
                    {/if}
                </Row>
                <Row>
                    <Column>
                        <WalterNumberInput
                            label="Jahr"
                            digits={0}
                            bind:value={newJahr}
                        />
                    </Column>
                    <Column>
                        <WalterNumberInput
                            label="Betrag (€)"
                            bind:value={newBetrag}
                        />
                    </Column>
                    <Column>
                        <WalterDatePicker
                            labelText="Datum"
                            bind:value={newDatum}
                        />
                    </Column>
                </Row>
                {#if submitError}
                    <InlineNotification
                        kind="error"
                        title="Fehler:"
                        subtitle={submitError}
                        hideCloseButton
                    />
                {/if}
                <Button
                    kind="primary"
                    size="small"
                    icon={Add}
                    disabled={!canSubmit}
                    on:click={submit}
                    style="margin-top: 0.5rem;"
                >
                    NK-Anteil anlegen
                </Button>
            {/if}
        </Tile>
    </AccordionItem>
</Accordion>
