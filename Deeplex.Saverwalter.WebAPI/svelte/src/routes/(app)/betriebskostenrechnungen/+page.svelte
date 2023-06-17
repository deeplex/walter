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
    umlagen_wohnungen={data.umlagen_wohnungen}
    betriebskostentypen={data.betriebskostentypen}
    search
    rows={data.rows}
    fullHeight
/>
