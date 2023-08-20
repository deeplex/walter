<script lang="ts">
    import {
        WalterHeaderDetail,
        WalterGrid,
        WalterZaehlerstand,
        WalterLinkTile
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
    <WalterZaehlerstand entry={data.entry} />
    <WalterLinkTile
        bind:fileWrapper
        name={`Zähler: ${data.entry.zaehler.text}`}
        href={`/zaehler/${data.entry.zaehler.id}`}
    />
</WalterGrid>
