<script lang="ts">
    import { WalterAnhaengeEntry, WalterHeader } from '$walter/components';
    import { Button, Content, InlineLoading, Loading } from 'carbon-components-svelte';
    import type { PageData } from './$types';
    import { WalterS3FileWrapper } from '$walter/lib';

    export let data: PageData;
    let fileWrapper = new WalterS3FileWrapper(data.fetchImpl);
    fileWrapper.register("Papierkorb", data.S3URL);

    const trashFiles = fileWrapper.handles[0].files;
</script>

<WalterHeader title="Papierkorb" />

<Content>
    <Button>Papierkorb leeren.</Button>
    {#await trashFiles}
        <Loading />
    {:then files}
        {#each files as file}
            <WalterAnhaengeEntry {file} bind:fileWrapper />
        {/each}
    {/await}
</Content> 