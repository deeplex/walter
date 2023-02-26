<script lang="ts">
	import { WalterHeader } from '../../components';
	import Vertraege from '../../components/lists/Vertraege.svelte';
	import { walter_get } from '../../services/utils';
	import type { VertragEntry } from '../../types/vertrag.type';

	const rows: Promise<VertragEntry[]> = walter_get('/api/vertraege');
</script>

<WalterHeader title="Verträge" />
<Vertraege search {rows} />
