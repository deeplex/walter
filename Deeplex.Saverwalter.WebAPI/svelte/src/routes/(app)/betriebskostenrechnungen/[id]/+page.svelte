<script lang="ts">
    import { Button } from 'carbon-components-svelte';
    import type { PageData } from './$types';

    import {
        WalterGrid,
        WalterWohnungen,
        WalterHeaderDetail,
        WalterBetriebskostenrechnung,
        WalterLinks
    } from '$walter/components';
    import { WalterS3FileWrapper } from '$walter/lib';

    export let data: PageData;

    const title =
        data.entry.typ?.text +
        ' - ' +
        data.entry.betreffendesJahr +
        ' - ' +
        data.entry.umlage?.text;

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
    <WalterBetriebskostenrechnung
        betriebskostentypen={data.betriebskostentypen}
        umlagen_wohnungen={data.umlagen_wohnungen}
        entry={data.entry}
    />

    <WalterLinks>
        <WalterWohnungen
            kontakte={data.kontakte}
            title="Wohnungen"
            rows={data.entry.wohnungen}
        />
    </WalterLinks>

    <Button href={`/umlagen/${data.entry.umlage?.id}`}>Zur Umlage</Button>
</WalterGrid>
