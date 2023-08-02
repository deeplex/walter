<script lang="ts">
    import {
        WalterBetriebskostenrechnungen,
        WalterHeaderList
    } from '$walter/components';
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<WalterHeaderList title="Betriebskostenrechnung" />
<WalterBetriebskostenrechnungen
    fetchImpl={data.fetchImpl}
    rows={data.rows}
    fullHeight
/>
