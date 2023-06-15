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
        data.a.betriebskostenrechnungen[
            data.a.betriebskostenrechnungen.length - 1
        ] || undefined;

    const umlage: WalterSelectionEntry = {
        id: '' + data.a.id,
        text: data.a.wohnungenBezeichnung,
        filter: '' + data.a.typ
    };
    const betriebskostenrechungEntry: Partial<WalterBetriebskostenrechnungEntry> =
        {
            typ: data.a.typ,
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
    files={data.anhaenge}
    a={data.a}
    apiURL={data.apiURL}
    title={`${data.a.typ.text} - ${data.a.wohnungenBezeichnung}`}
    fetchImpl={data.fetch}
/>

<WalterGrid>
    <WalterUmlage
        zaehler={data.zaehler}
        betriebskostentypen={data.betriebskostentypen}
        wohnungen={data.wohnungen}
        umlageschluessel={data.umlageschluessel}
        bind:a={data.a}
    />

    <WalterLinks>
        <WalterWohnungen
            kontakte={data.kontakte}
            title="Wohnungen"
            rows={data.a.wohnungen}
        />
        <WalterBetriebskostenrechnungen
            betriebskostentypen={data.betriebskostentypen}
            umlagen_wohnungen={data.umlagen_wohnungen}
            a={betriebskostenrechungEntry}
            title="Betriebskostenrechnungen"
            rows={data.a.betriebskostenrechnungen}
        />
        <!-- Only show if Schlüssel is "nach Verbrauch" -->
        {#if data.a?.schluessel?.id === '3'}
            <WalterZaehlerList
                title="Zähler"
                wohnungen={data.wohnungen}
                umlagen={data.umlagen}
                zaehlertypen={data.zaehlertypen}
                rows={data.a.zaehler}
            />
        {/if}
    </WalterLinks>
</WalterGrid>
