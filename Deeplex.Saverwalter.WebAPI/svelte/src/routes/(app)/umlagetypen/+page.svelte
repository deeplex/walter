<script lang="ts">
    import { WalterHeader } from '$walter/components';
    import WalterUmlagetypen from '$walter/components/lists/WalterUmlagetypen.svelte';
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<WalterHeader title="Umlagetypen" />
<WalterUmlagetypen fetchImpl={data.fetchImpl} rows={data.rows} fullHeight />
