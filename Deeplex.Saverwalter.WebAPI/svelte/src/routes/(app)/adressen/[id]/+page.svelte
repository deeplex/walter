<script lang="ts">
    import {
        WalterAdresse,
        WalterGrid,
        WalterHeaderDetail,
        WalterKontakte,
        WalterLinks,
        WalterWohnungen,
        WalterZaehlerList
    } from '$walter/components';
    import type { WalterWohnungEntry } from '$walter/lib';
    import type { PageData } from './$types';

    export let data: PageData;
    const wohnungEntry: Partial<WalterWohnungEntry> = {
        adresse: { ...data.entry }
    };
</script>

<WalterHeaderDetail
    S3URL={data.S3URL}
    files={data.anhaenge}
    entry={data.entry}
    apiURL={data.apiURL}
    title={data.entry.anschrift}
    fetchImpl={data.fetch}
/>

<WalterGrid>
    <WalterAdresse bind:value={data.entry} />

    <WalterLinks>
        <WalterWohnungen
            kontakte={data.kontakte}
            entry={wohnungEntry}
            title="Wohnungen"
            rows={data.entry.wohnungen}
        />
        <WalterKontakte title="Personen" rows={data.entry.kontakte} />
        <WalterZaehlerList
            umlagen={data.umlagen}
            zaehlertypen={data.zaehlertypen}
            wohnungen={data.wohnungen}
            title="Zähler"
            rows={data.entry.zaehler}
        />
    </WalterLinks>
</WalterGrid>
