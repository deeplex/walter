<script lang="ts">
    import { WalterAccounts, WalterHeader } from '$walter/components';
    import { walter_goto } from '$walter/services/utils';
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<WalterHeader title="Nutzereinstellungen" />
<WalterAccounts fetchImpl={data.fetchImpl} rows={data.accounts} fullHeight />
