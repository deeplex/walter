<script lang="ts">
    import { WalterHeaderList, WalterWohnungen } from '$walter/components';
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<WalterHeaderList title="Wohnungen" />
<WalterWohnungen kontakte={data.kontakte} search rows={data.rows} fullHeight />
