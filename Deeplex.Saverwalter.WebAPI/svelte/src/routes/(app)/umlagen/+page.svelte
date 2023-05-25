<script lang="ts">
    import { WalterUmlagen, WalterHeaderList } from '$WalterComponents';
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<WalterHeaderList title="Umlagen" />
<WalterUmlagen
    wohnungen={data.wohnungen}
    umlageschluessel={data.umlageschluessel}
    betriebskostentypen={data.betriebskostentypen}
    search
    rows={data.rows}
/>
