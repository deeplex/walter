<script lang="ts">
    import { WalterUmlagen, WalterHeaderList } from '$walter/components';
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<WalterHeaderList title="Umlagen" />
<WalterUmlagen
    zaehler={data.zaehler}
    wohnungen={data.wohnungen}
    umlageschluessel={data.umlageschluessel}
    betriebskostentypen={data.betriebskostentypen}
    search
    rows={data.rows}
    fullHeight
/>
