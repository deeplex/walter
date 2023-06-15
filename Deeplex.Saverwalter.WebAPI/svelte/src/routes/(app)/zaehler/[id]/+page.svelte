<script lang="ts">
    import type { PageData } from './$types';
    import {
        WalterHeaderDetail,
        WalterGrid,
        WalterZaehler,
        WalterZaehlerstaende,
        WalterUmlagen,
        WalterLinks
    } from '$walter/components';
    import { convertDateCanadian } from '$walter/services/utils';
    import type { WalterZaehlerstandEntry } from '$walter/lib';

    export let data: PageData;

    const staende = data.entry.staende;

    const lastZaehlerstand = staende.length
        ? staende[staende.length - 1]
        : undefined;
    const zaehlerstandEntry: Partial<WalterZaehlerstandEntry> = {
        zaehler: { id: '' + data.entry.id, text: data.entry.kennnummer },
        datum: convertDateCanadian(new Date()),
        stand: lastZaehlerstand?.stand || 0,
        einheit: lastZaehlerstand?.einheit
    };
</script>

<WalterHeaderDetail
    S3URL={data.S3URL}
    files={data.anhaenge}
    entry={data.entry}
    apiURL={data.apiURL}
    title={data.entry.kennnummer}
    fetchImpl={data.fetch}
/>

<WalterGrid>
    <WalterZaehler
        umlagen={data.umlagen}
        zaehlertypen={data.zaehlertypen}
        wohnungen={data.wohnungen}
        entry={data.entry}
    />

    <WalterLinks>
        <WalterZaehlerstaende
            entry={zaehlerstandEntry}
            title="Zählerstände"
            rows={data.entry.staende}
        />
        <WalterUmlagen
            title="Umlagen"
            zaehler={data.zaehler}
            umlageschluessel={data.umlageschluessel}
            wohnungen={data.wohnungen}
            betriebskostentypen={data.betriebskostentypen}
            rows={data.entry.umlagen}
        />
    </WalterLinks>
</WalterGrid>
