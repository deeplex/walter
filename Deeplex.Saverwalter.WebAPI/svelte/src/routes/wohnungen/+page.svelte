<script lang="ts">
	import { WalterHeader } from '../../components';
	import Wohnungen from '../../components/lists/Wohnungen.svelte';
	import { walter_get } from '../../services/utils';
	import type { WohnungListEntry } from '../../types/wohnunglist.type';

	const rows: Promise<WohnungListEntry[]> = walter_get('/api/wohnungen');
</script>

<WalterHeader title="Wohnungen" />
<Wohnungen search {rows} />
