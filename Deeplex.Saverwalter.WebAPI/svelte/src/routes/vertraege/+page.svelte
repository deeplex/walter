<script lang="ts">
	import { WalterHeader } from '../../components';
	import Vertraege from '../../components/lists/Vertraege.svelte';
	import { walter_get } from '../../services/utils';
	import type { VertragListEntry } from '../../types/vertraglist.type';

	const rows: Promise<VertragListEntry[]> = walter_get('/api/vertraege');
</script>

<WalterHeader title="Verträge" />
<Vertraege search {rows} />
