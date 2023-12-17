<script lang="ts">
    import {
        WalterHeaderDetail,
        WalterGrid,
        WalterMietminderung,
        WalterLinkTile
    } from '$walter/components';
    import type { PageData } from './$types';
    import { WalterS3FileWrapper } from '$walter/lib';
    import { S3URL } from '$walter/services/s3';

    export let data: PageData;

    let title = data.entry.vertrag.text;
    $: {
        title = data.entry.vertrag.text;
    }

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
    <WalterMietminderung entry={data.entry} />

    <WalterLinkTile
        bind:fileWrapper
        s3ref={S3URL.adresse(`${data.entry.vertrag.id}`)}
        name={`Vertrag: ${data.entry.vertrag.text}`}
        href={`/vertraege/${data.entry.vertrag.id}`}
    />
</WalterGrid>
