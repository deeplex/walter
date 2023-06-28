<script lang="ts">
    import {
        WalterHeaderDetail,
        WalterGrid,
        WalterVertragVersion,
        WalterLink
    } from '$walter/components';
    import type { PageData } from './$types';
    import { WalterS3FileWrapper } from '$walter/lib';

    export let data: PageData;

    const title = data.entry.vertrag.text;
    let fileWrapper = new WalterS3FileWrapper(data.fetch);
    fileWrapper.register(title, data.S3URL);
</script>

<WalterHeaderDetail
    entry={data.entry}
    apiURL={data.apiURL}
    {title}
    bind:fileWrapper
/>

<WalterGrid>
    <WalterVertragVersion entry={data.entry} />
    <WalterLink
        bind:fileWrapper
        name={`Vertrag: ${data.entry.vertrag.text}`}
        href={`/vertraege/${data.entry.vertrag.id}`}
    />
</WalterGrid>
