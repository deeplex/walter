<script lang="ts">
    import {
        WalterBetriebskostenrechnungen,
        WalterHeader
    } from '$walter/components';
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<WalterHeader title="Betriebskostenrechnung" />
<WalterBetriebskostenrechnungen
    fetchImpl={data.fetchImpl}
    rows={data.rows}
    fullHeight
/>
