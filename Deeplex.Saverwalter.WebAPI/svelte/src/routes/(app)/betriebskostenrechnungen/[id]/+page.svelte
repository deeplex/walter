<script lang="ts">
    import type { PageData } from './$types';

    import {
        WalterGrid,
        WalterWohnungen,
        WalterHeaderDetail,
        WalterBetriebskostenrechnung,
        WalterLinks,
        WalterLink
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

        <WalterLink
            {fileWrapper}
            name={`Umlage: ${data.entry.umlage.text}`}
            href={`/umlagen/${data.entry.umlage?.id}`}
        />
    </WalterLinks>
</WalterGrid>
