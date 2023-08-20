<script lang="ts">
    import {
        WalterHeaderDetail,
        WalterGrid,
        WalterMietminderung,
        WalterLinkTile
    } from '$walter/components';
    import { Button, ButtonSkeleton } from 'carbon-components-svelte';
    import type { PageData } from './$types';
    import { WalterS3FileWrapper } from '$walter/lib';

    export let data: PageData;

    const title = data.entry.vertrag.text;
    let fileWrapper = new WalterS3FileWrapper(data.fetchImpl);
    fileWrapper.registerStack()
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
        name={`Vertrag: ${data.entry.vertrag.text}`}
        href={`/vertraege/${data.entry.vertrag.id}`}
    />
</WalterGrid>
