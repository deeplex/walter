<script lang="ts">
    import {
        WalterGrid,
        WalterZaehler,
        WalterHeaderNew
    } from '$walter/components';
    import type { WalterZaehlerEntry } from '$walter/lib';
    import type { PageData } from './$types';

    export let data: PageData;

    const entry: Partial<WalterZaehlerEntry> = {};
</script>

<WalterHeaderNew apiURL={data.apiURL} {entry} title={data.title} />

<WalterGrid>
    <WalterZaehler fetchImpl={data.fetchImpl} {entry} />
</WalterGrid>
