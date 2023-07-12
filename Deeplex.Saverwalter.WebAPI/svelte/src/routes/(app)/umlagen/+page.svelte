<script lang="ts">
    import { WalterUmlagen, WalterHeaderList } from '$walter/components';
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<WalterHeaderList title="Umlagen" />
<WalterUmlagen fetchImpl={data.fetchImpl} search rows={data.rows} fullHeight />
