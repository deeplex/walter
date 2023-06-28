<script lang="ts">
    import {
        WalterHeaderDetail,
        WalterGrid,
        WalterMietminderung
    } from '$walter/components';
    import { Button, ButtonSkeleton } from 'carbon-components-svelte';
    import type { PageData } from './$types';
    import { WalterS3FileWrapper } from '$walter/lib';

    export let data: PageData;

    const title = data.entry.vertrag.text;
    const fileWrapper = new WalterS3FileWrapper(data.fetch);
    fileWrapper.register(title, data.S3URL);
</script>

<WalterHeaderDetail
    entry={data.entry}
    apiURL={data.apiURL}
    {title}
    {fileWrapper}
/>

<WalterGrid>
    <WalterMietminderung entry={data.entry} />
    {#await data.entry}
        <ButtonSkeleton />
    {:then x}
        <Button href={`/vertraege/${x.vertrag.id}`}>Zum Vertrag</Button>
    {/await}
</WalterGrid>
