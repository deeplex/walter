<script lang="ts">
    import {
        WalterGrid,
        WalterUmlage,
        WalterHeaderNew
    } from '$walter/components';
    import type { WalterUmlageEntry } from '$walter/lib';
    import type { PageData } from './$types';

    const entry: Partial<WalterUmlageEntry> = {};

    export let data: PageData;
</script>

<WalterHeaderNew apiURL={data.apiURL} {entry} title={data.title} />

<WalterGrid>
    <WalterUmlage
        zaehler={data.zaehler}
        wohnungen={data.wohnungen}
        betriebskostentypen={data.betriebskostentypen}
        umlageschluessel={data.umlageschluessel}
        {entry}
    />
</WalterGrid>
