<script lang="ts">
    import { WalterAccounts, WalterHeader } from '$walter/components';
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<WalterHeader title="Nutzereinstellungen" />
<WalterAccounts fetchImpl={data.fetchImpl} rows={data.accounts} fullHeight />
