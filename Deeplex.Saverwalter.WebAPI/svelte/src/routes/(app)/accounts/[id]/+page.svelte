<script lang="ts">
    import { WalterGrid, WalterHeaderDetail } from '$walter/components';
    import WalterAccount from '$walter/components/details/WalterAccount.svelte';
    import { WalterS3FileWrapper } from '$walter/lib';
    import type { PageData } from './$types';

    export let data: PageData;

    const title = `${data.entry.username} - ${data.entry.name}`;

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
    <WalterAccount admin entry={data.entry} fetchImpl={data.fetchImpl} />
</WalterGrid>
