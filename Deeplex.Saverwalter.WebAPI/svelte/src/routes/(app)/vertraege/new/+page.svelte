<script lang="ts">
    import {
        WalterGrid,
        WalterVertrag,
        WalterHeaderNew,
        WalterVertragVersion
    } from '$walter/components';
    import type {
        WalterVertragEntry,
        WalterVertragVersionEntry
    } from '$walter/lib';
    import { Tile } from 'carbon-components-svelte';
    import type { PageData } from './$types';

    export let data: PageData;

    let entryVersion: Partial<WalterVertragVersionEntry> = {};
    let entry: Partial<WalterVertragEntry> = {
        versionen: [entryVersion as WalterVertragVersionEntry]
    };
</script>

<WalterHeaderNew apiURL={data.apiURL} {entry} title={data.title} />

<WalterGrid>
    <WalterVertrag
        kontakte={data.kontakte}
        wohnungen={data.wohnungen}
        a={entry}
    />
    <Tile light>Vertragsversion:</Tile>
    <WalterVertragVersion a={entryVersion} />
</WalterGrid>
