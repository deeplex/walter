<script lang="ts">
	import { walter_get } from '../../services/utils';
	import { WalterHeader } from '../../components';
	import Kontakte from '../../components/lists/Kontakte.svelte';
	import type { PersonEntry } from '../../types/person.type';

	const rows: Promise<PersonEntry[]> = walter_get('/api/kontakte');
</script>

<WalterHeader title="Kontakte" />
<Kontakte search {rows} />
