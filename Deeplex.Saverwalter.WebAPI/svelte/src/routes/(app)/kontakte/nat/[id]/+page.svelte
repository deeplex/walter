<script lang="ts">
    import type { PageData } from './$types';
    import {
        WalterGrid,
        WalterKontakte,
        WalterWohnungen,
        WalterVertraege,
        WalterHeaderDetail,
        WalterNatuerlichePerson,
        WalterLinks,
        WalterLink
    } from '$walter/components';
    import { WalterS3FileWrapper } from '$walter/lib';

    export let data: PageData;

    const title = data.entry.name;
    let fileWrapper = new WalterS3FileWrapper(data.fetchImpl);
    fileWrapper.register(title, data.S3URL);
</script>

<WalterHeaderDetail
    entry={data.entry}
    apiURL={data.apiURL}
    {title}
    bind:fileWrapper
/>

<WalterGrid>
    <WalterNatuerlichePerson
        bind:entry={data.entry}
        fetchImpl={data.fetchImpl}
    />

    <WalterLinks>
        <WalterKontakte
            title="Juristische Personen"
            rows={data.entry.juristischePersonen}
        />
        <WalterWohnungen
            fetchImpl={data.fetchImpl}
            title="Wohnungen"
            rows={data.entry.wohnungen}
        />
        <WalterVertraege
            fetchImpl={data.fetchImpl}
            title="Verträge"
            rows={data.entry.vertraege}
        />

        <WalterLink
            bind:fileWrapper
            name={`Adresse: ${data.entry.adresse.anschrift}`}
            href={`/adressen/${data.entry.adresse.id}`}
        />
    </WalterLinks>
</WalterGrid>
