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
    umlagen={data.umlagen}
    betriebskostentypen={data.betriebskostentypen}
    search
    rows={data.rows}
/>
