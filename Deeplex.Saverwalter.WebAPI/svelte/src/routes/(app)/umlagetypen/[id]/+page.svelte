<script lang="ts">
    import type { PageData } from './$types';
    import {
        WalterHeaderDetail,
        WalterGrid,
        WalterLinks,
        WalterUmlagetyp,
        WalterUmlagen
    } from '$walter/components';
    import { WalterS3FileWrapper } from '$walter/lib';

    export let data: PageData;

    const title = data.entry.bezeichnung;
    let fileWrapper = new WalterS3FileWrapper(data.fetchImpl);
    fileWrapper.registerStack();
    fileWrapper.register(title, data.S3URL);
</script>

<WalterHeaderDetail
    entry={data.entry}
    apiURL={data.apiURL}
    title={data.entry.bezeichnung}
    bind:fileWrapper
/>

<WalterGrid>
    <WalterUmlagetyp fetchImpl={data.fetchImpl} bind:entry={data.entry} />
    <WalterLinks>
        <WalterUmlagen
            fetchImpl={data.fetchImpl}
            title="Umlagen"
            rows={data.entry.umlagen}
        />
    </WalterLinks>
</WalterGrid>
