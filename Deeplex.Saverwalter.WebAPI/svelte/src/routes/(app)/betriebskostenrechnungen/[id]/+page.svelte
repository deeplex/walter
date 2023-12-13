<script lang="ts">
    import type { PageData } from './$types';

    import {
        WalterGrid,
        WalterWohnungen,
        WalterHeaderDetail,
        WalterBetriebskostenrechnung,
        WalterBetriebskostenrechnungen,
        WalterLinks,
        WalterLinkTile
    } from '$walter/components';
    import { WalterS3FileWrapper } from '$walter/lib';

    export let data: PageData;

    let title =
        data.entry.typ?.text +
        ' - ' +
        data.entry.betreffendesJahr +
        ' - ' +
        data.entry.umlage?.text;
    $: {
        title =
            data.entry.typ?.text +
            ' - ' +
            data.entry.betreffendesJahr +
            ' - ' +
            data.entry.umlage?.text;
    }

    let fileWrapper = new WalterS3FileWrapper(data.fetchImpl);
    fileWrapper.registerStack();
    fileWrapper.register(title, data.S3URL);
</script>

<WalterHeaderDetail
    entry={data.entry}
    apiURL={data.apiURL}
    {title}
    bind:fileWrapper
/>

<WalterGrid>
    <WalterBetriebskostenrechnung
        fetchImpl={data.fetchImpl}
        entry={data.entry}
    />

    <WalterLinks>
        <WalterWohnungen
            fetchImpl={data.fetchImpl}
            title="Wohnungen"
            rows={data.entry.wohnungen}
        />

        <WalterBetriebskostenrechnungen
            fetchImpl={data.fetchImpl}
            title="Rechnungen der Umlage"
            rows={data.entry.betriebskostenrechnungen}
        />

        <WalterLinkTile
            bind:fileWrapper
            name={`Umlage: ${data.entry.umlage.text}`}
            href={`/umlagen/${data.entry.umlage?.id}`}
        />
    </WalterLinks>
</WalterGrid>
