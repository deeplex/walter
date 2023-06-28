<script lang="ts">
    import {
        WalterHeaderDetail,
        WalterGrid,
        WalterMiete,
        WalterLink
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
    <WalterMiete entry={data.entry} />

    <WalterLink
        {fileWrapper}
        name={`Adresse: ${data.entry.vertrag.id}`}
        href={`/adresse/${data.entry.vertrag.text}`}
    />
</WalterGrid>
