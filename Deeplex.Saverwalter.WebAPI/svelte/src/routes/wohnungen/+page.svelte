<script lang="ts">
	import { WalterHeaderList, WalterWohnungen } from '$WalterComponents';
	import { walter_get } from '$WalterServices/requests';
	import type { WalterWohnungEntry } from '$WalterTypes';

	const url = 'wohnungen';

	const rows: Promise<WalterWohnungEntry[]> = walter_get(`/api/${url}`);
</script>

<WalterHeaderList {url} title="Wohnungen" />
<WalterWohnungen search {rows} />
