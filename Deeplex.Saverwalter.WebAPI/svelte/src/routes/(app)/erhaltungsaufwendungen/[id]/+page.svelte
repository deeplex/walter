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
    <WalterErhaltungsaufwendung
        kontakte={data.kontakte}
        wohnungen={data.wohnungen}
        entry={data.entry}
    />
</WalterGrid>

<WalterLinks>
    <WalterLink
        {fileWrapper}
        name={`Wohnung: ${data.entry.wohnung.text}`}
        href={`/wohnungen/${data.entry.wohnung.id}`}
    />
    <WalterLink
        {fileWrapper}
        name={`Aussteller: ${data.entry.aussteller.text}`}
        href={`/kontakte/jur/${data.entry.aussteller.id}`}
    />
</WalterLinks>
