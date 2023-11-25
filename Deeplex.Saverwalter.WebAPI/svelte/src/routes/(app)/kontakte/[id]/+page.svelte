<script lang="ts">
    import type { PageData } from './$types';
    import {
        WalterGrid,
        WalterKontakte,
        WalterWohnungen,
        WalterVertraege,
        WalterHeaderDetail,
        WalterLinks,
        WalterLinkTile,
        WalterKontakt
    } from '$walter/components';
    import { WalterS3FileWrapper } from '$walter/lib';

    export let data: PageData;

    const title = data.entry.name;
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
    <WalterKontakt
        bind:entry={data.entry}
        fetchImpl={data.fetchImpl}
    />

    <WalterLinks>
        <WalterKontakte
            fetchImpl={data.fetchImpl}
            title="Juristische Personen"
            rows={data.entry.juristischePersonen}
        />
        {#if data.entry.rechtsform.id !== 0}
            <WalterKontakte
            fetchImpl={data.fetchImpl}
            title="Mitglieder"
            rows={data.entry.mitglieder}
    />
        {/if}
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

        {#if data.entry.adresse}
            <WalterLinkTile
                bind:fileWrapper
                name={`Adresse: ${data.entry.adresse.anschrift}`}
                href={`/adressen/${data.entry.adresse.id}`}
            />
        {/if}
    </WalterLinks>
</WalterGrid>
