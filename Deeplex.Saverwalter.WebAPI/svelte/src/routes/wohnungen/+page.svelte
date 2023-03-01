<script lang="ts">
	import { WalterHeader, Wohnungen } from '$components';
	import { walter_get } from '$services/utils';
	import type { WohnungEntry } from '$types';

	const rows: Promise<WohnungEntry[]> = walter_get('/api/wohnungen');
</script>

<WalterHeader title="Wohnungen" />
<Wohnungen search {rows} />
