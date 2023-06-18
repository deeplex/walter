<script lang="ts">
    import type { PageData } from './$types';
    import {
        WalterGrid,
        WalterKontakte,
        WalterWohnungen,
        WalterVertraege,
        WalterHeaderDetail,
        WalterNatuerlichePerson,
        WalterLinks
    } from '$walter/components';

    export let data: PageData;
</script>

<WalterHeaderDetail
    S3URL={data.S3URL}
    files={data.files}
    entry={data.entry}
    apiURL={data.apiURL}
    title={data.entry.name}
    fetchImpl={data.fetch}
/>

<WalterGrid>
    <WalterNatuerlichePerson
        bind:entry={data.entry}
        juristischePersonen={data.juristischePersonen}
    />

    <WalterLinks>
        <WalterKontakte
            title="Juristische Personen"
            rows={data.entry.juristischePersonen}
        />
        <WalterWohnungen
            kontakte={data.kontakte}
            title="Wohnungen"
            rows={data.entry.wohnungen}
        />
        <WalterVertraege
            wohnungen={data.wohnungen}
            kontakte={data.kontakte}
            title="Verträge"
            rows={data.entry.vertraege}
        />
    </WalterLinks>
</WalterGrid>
