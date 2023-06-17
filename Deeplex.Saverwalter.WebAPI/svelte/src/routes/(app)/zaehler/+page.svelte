<script lang="ts">
    import { WalterHeaderList, WalterZaehlerList } from '$walter/components';
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<WalterHeaderList title="Zähler" />
<WalterZaehlerList
    zaehlertypen={data.zaehlertypen}
    wohnungen={data.wohnungen}
    umlagen={data.umlagen}
    search
    rows={data.rows}
    fullHeight
/>
