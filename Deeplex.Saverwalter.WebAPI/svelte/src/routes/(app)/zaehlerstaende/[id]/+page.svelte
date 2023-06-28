<script lang="ts">
    import {
        WalterHeaderDetail,
        WalterGrid,
        WalterZaehlerstand
    } from '$walter/components';
    import { convertDateGerman } from '$walter/services/utils';
    import { Button, ButtonSkeleton } from 'carbon-components-svelte';
    import type { PageData } from './$types';
    import { WalterS3FileWrapper } from '$walter/lib';

    export let data: PageData;

    const title =
        data.entry.zaehler.text +
        ' - ' +
        convertDateGerman(new Date(data.entry.datum));
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
    <WalterZaehlerstand entry={data.entry} />
    {#await data.entry}
        <ButtonSkeleton />
    {:then x}
        <Button href={`/zaehler/${x.zaehler.id}`}>Zum Zähler</Button>
    {/await}
</WalterGrid>
