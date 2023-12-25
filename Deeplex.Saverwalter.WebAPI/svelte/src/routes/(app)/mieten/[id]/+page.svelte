<script lang="ts">
    import {
        WalterHeaderDetail,
        WalterGrid,
        WalterMiete
    } from '$walter/components';
    import type { PageData } from './$types';
    import { WalterFileWrapper } from '$walter/lib';

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
    <WalterMiete entry={data.entry} mieten={data.vertrag.mieten} />
</WalterGrid>
