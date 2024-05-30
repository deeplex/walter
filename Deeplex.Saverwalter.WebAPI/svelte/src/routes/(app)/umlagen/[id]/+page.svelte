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

<script lang="ts">
    import type { PageData } from './$types';
    import {
        WalterBetriebskostenrechnungen,
        WalterHeaderDetail,
        WalterGrid,
        WalterWohnungen,
        WalterUmlage,
        WalterZaehlerList,
        WalterLinks,
        WalterLinkTile,
        WalterZaehler
    } from '$walter/components';
    import { convertDateCanadian } from '$walter/services/utils';
    import {
        WalterFileWrapper,
        type WalterBetriebskostenrechnungEntry,
        type WalterSelectionEntry
    } from '$walter/lib';
    import { Row } from 'carbon-components-svelte';
    import WalterDataLineChart from '$walter/components/data/WalterDataLineChart.svelte';
    import { walter_data_rechnungen_year } from '$walter/components/data/WalterData';
    import { fileURL } from '$walter/services/files';

    export let data: PageData;

    const lastBetriebskostenrechnung =
        data.entry.betriebskostenrechnungen[
            data.entry.betriebskostenrechnungen.length - 1
        ] || undefined;

    const umlage: WalterSelectionEntry = {
        id: '' + data.entry.id,
        text: data.entry.wohnungenBezeichnung,
        filter: '' + data.entry.typ
    };
    const betriebskostenrechungEntry: Partial<WalterBetriebskostenrechnungEntry> =
        {
            typ: data.entry.typ,
            umlage: umlage,
            betrag: lastBetriebskostenrechnung?.betrag || 0,
            betreffendesJahr:
                lastBetriebskostenrechnung?.betreffendesJahr + 1 ||
                new Date().getFullYear(),
            datum: convertDateCanadian(new Date()),
            permissions: data.entry.permissions
        };

    let title = `${data.entry.typ.text} - ${data.entry.wohnungenBezeichnung}`;
    $: {
        title = `${data.entry.typ.text} - ${data.entry.wohnungenBezeichnung}`;
    }

    let fileWrapper = new WalterFileWrapper(data.fetchImpl);
    fileWrapper.registerStack();
    fileWrapper.register(title, data.fileURL);
</script>

<WalterHeaderDetail
    entry={data.entry}
    apiURL={data.apiURL}
    {title}
    bind:fileWrapper
/>

<WalterGrid>
    <WalterUmlage fetchImpl={data.fetchImpl} bind:entry={data.entry} />

    <WalterLinks>
        <WalterWohnungen
            fetchImpl={data.fetchImpl}
            title="Wohnungen"
            rows={data.entry.wohnungen}
        />
        <WalterBetriebskostenrechnungen
            fetchImpl={data.fetchImpl}
            entry={betriebskostenrechungEntry}
            title="Rechnungen"
            rows={data.entry.betriebskostenrechnungen}
        />
        <!-- Only show if Schlüssel is "nach Verbrauch" -->
        {#if `${data.entry?.schluessel?.id}` === '3'}
            <WalterZaehlerList
                fetchImpl={data.fetchImpl}
                title="Zähler"
                rows={data.entry.zaehler}
            />
        {/if}
        <WalterLinkTile
            {fileWrapper}
            fileref={fileURL.umlagetyp(`${data.entry.typ.id}`)}
            name={`Umlagetyp ansehen`}
            href={`/umlagetypen/${data.entry.typ.id}`}
        />
    </WalterLinks>

    {#if data.entry.betriebskostenrechnungen.length > 1}
        <Row>
            <WalterDataLineChart
                config={walter_data_rechnungen_year(
                    'Rechnungen',
                    data.entry.typ.text,
                    data.entry.betriebskostenrechnungen
                )}
            />
        </Row>
    {/if}
</WalterGrid>
