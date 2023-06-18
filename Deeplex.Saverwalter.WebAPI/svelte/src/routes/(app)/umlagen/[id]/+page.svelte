<script lang="ts">
    import type { PageData } from './$types';
    import {
        WalterBetriebskostenrechnungen,
        WalterHeaderDetail,
        WalterGrid,
        WalterWohnungen,
        WalterUmlage,
        WalterZaehlerList,
        WalterLinks
    } from '$walter/components';
    import { convertDateCanadian } from '$walter/services/utils';
    import type {
        WalterBetriebskostenrechnungEntry,
        WalterSelectionEntry
    } from '$walter/lib';

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
            datum: convertDateCanadian(new Date())
        };
</script>

<WalterHeaderDetail
    S3URL={data.S3URL}
    files={data.files}
    entry={data.entry}
    apiURL={data.apiURL}
    title={`${data.entry.typ.text} - ${data.entry.wohnungenBezeichnung}`}
    fetchImpl={data.fetch}
/>

<WalterGrid>
    <WalterUmlage
        zaehler={data.zaehler}
        betriebskostentypen={data.betriebskostentypen}
        wohnungen={data.wohnungen}
        umlageschluessel={data.umlageschluessel}
        bind:entry={data.entry}
    />

    <WalterLinks>
        <WalterWohnungen
            kontakte={data.kontakte}
            title="Wohnungen"
            rows={data.entry.wohnungen}
        />
        <WalterBetriebskostenrechnungen
            betriebskostentypen={data.betriebskostentypen}
            umlagen_wohnungen={data.umlagen_wohnungen}
            entry={betriebskostenrechungEntry}
            title="Betriebskostenrechnungen"
            rows={data.entry.betriebskostenrechnungen}
        />
        <!-- Only show if Schlüssel is "nach Verbrauch" -->
        {#if data.entry?.schluessel?.id === '3'}
            <WalterZaehlerList
                title="Zähler"
                wohnungen={data.wohnungen}
                umlagen={data.umlagen}
                zaehlertypen={data.zaehlertypen}
                rows={data.entry.zaehler}
            />
        {/if}
    </WalterLinks>
</WalterGrid>
