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
        WalterLinkTile
    } from '$walter/components';
    import { convertDateCanadian } from '$walter/services/utils';
    import {
        WalterS3FileWrapper,
        type WalterBetriebskostenrechnungEntry,
        type WalterSelectionEntry
    } from '$walter/lib';
    import { Row } from 'carbon-components-svelte';
    import WalterDataLineChart from '$walter/components/data/WalterDataLineChart.svelte';
    import { walter_data_rechnungen_year } from '$walter/components/data/WalterData';

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

    const title = `${data.entry.typ.text} - ${data.entry.wohnungenBezeichnung}`;
    let fileWrapper = new WalterS3FileWrapper(data.fetchImpl);
    fileWrapper.registerStack();
    fileWrapper.register(title, data.S3URL);
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
        {#if data.entry?.schluessel?.id === '3'}
            <WalterZaehlerList
                fetchImpl={data.fetchImpl}
                title="Zähler"
                rows={data.entry.zaehler}
            />
        {/if}
        <WalterLinkTile
            {fileWrapper}
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
