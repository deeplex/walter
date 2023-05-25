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
    } from '$WalterComponents';

    export let data: PageData;
</script>

<WalterHeaderDetail
    S3URL={data.S3URL}
    files={data.anhaenge}
    a={data.a}
    apiURL={data.apiURL}
    title={data.a.name}
    f={data.fetch}
/>

<WalterGrid>
    <WalterNatuerlichePerson
        bind:a={data.a}
        juristischePersonen={data.juristischePersonen}
    />

    <WalterLinks>
        <WalterKontakte
            title="Juristische Personen"
            rows={data.a.juristischePersonen}
        />
        <WalterWohnungen
            kontakte={data.kontakte}
            title="Wohnungen"
            rows={data.a.wohnungen}
        />
        <WalterVertraege
            wohnungen={data.wohnungen}
            kontakte={data.kontakte}
            title="Verträge"
            rows={data.a.vertraege}
        />
    </WalterLinks>
</WalterGrid>
