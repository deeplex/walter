<script lang="ts">
    import type { PageData } from './$types';
    import {
        WalterHeaderDetail,
        WalterGrid,
        WalterErhaltungsaufwendung,
        WalterLink,
        WalterLinks
    } from '$walter/components';
    import { WalterS3FileWrapper } from '$walter/lib';

    export let data: PageData;

    const title = data.entry.aussteller?.text + ' - ' + data.entry.bezeichnung;

    let fileWrapper = new WalterS3FileWrapper(data.fetchImpl);
    fileWrapper.register(title, data.S3URL);
</script>

<WalterHeaderDetail
    entry={data.entry}
    apiURL={data.apiURL}
    {title}
    bind:fileWrapper
/>

<WalterGrid>
    <WalterErhaltungsaufwendung fetchImpl={data.fetchImpl} entry={data.entry} />
</WalterGrid>

<WalterLinks>
    <WalterLink
        bind:fileWrapper
        name={`Wohnung: ${data.entry.wohnung.text}`}
        href={`/wohnungen/${data.entry.wohnung.id}`}
    />
    <WalterLink
        bind:fileWrapper
        name={`Aussteller: ${data.entry.aussteller.text}`}
        href={`/kontakte/jur/${data.entry.aussteller.id}`}
    />
</WalterLinks>
