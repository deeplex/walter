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

    export let data: PageData;
</script>

<WalterHeaderDetail
    S3URL={data.S3URL}
    files={data.files}
    refFiles={data.refFiles}
    entry={data.entry}
    apiURL={data.apiURL}
    title={data.entry.typ?.text +
        ' - ' +
        data.entry.betreffendesJahr +
        ' - ' +
        data.entry.umlage?.text}
    fetchImpl={data.fetch}
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
