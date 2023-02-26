<script lang="ts">
	import { WalterHeader } from '../../components';
	import Wohnungen from '../../components/lists/Wohnungen.svelte';
	import { walter_get } from '../../services/utils';
	import type { WohnungEntry } from '../../types/wohnung.type';

	const rows: Promise<WohnungEntry[]> = walter_get('/api/wohnungen');
</script>

<WalterHeader title="Wohnungen" />
<Wohnungen search {rows} />
