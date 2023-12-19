<script lang="ts">
    import {
        WalterHeaderDetail,
        WalterGrid,
        WalterVertragVersion,
        WalterLinkTile
    } from '$walter/components';
    import type { PageData } from './$types';
    import { WalterFileWrapper } from '$walter/lib';
    import { fileURL } from '$walter/services/files';

    export let data: PageData;

    let title = data.entry.vertrag.text;
    $: {
        title = data.entry.vertrag.text;
    }

    let fileWrapper = new WalterFileWrapper(data.fetchImpl);
    fileWrapper.registerStack();
    fileWrapper.register(title, data.fileURL);
</script>

<WalterHeaderDetail
    entry={data.entry}
    apiURL={data.apiURL}
    {title}
    bind:fileWrapper
/>

<WalterGrid>
    <WalterVertragVersion entry={data.entry} />
    <WalterLinkTile
        bind:fileWrapper
        fileref={fileURL.vertrag(`${data.entry.vertrag.id}`)}
        name={`Vertrag: ${data.entry.vertrag.text}`}
        href={`/vertraege/${data.entry.vertrag.id}`}
    />
</WalterGrid>
