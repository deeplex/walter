<script lang="ts">
	import { Erhaltungsaufwendungen, WalterHeader } from '$components';
	import { walter_get } from '$services/utils';
	import type { ErhaltungsaufwendungEntry } from '$types';

	const rows: Promise<ErhaltungsaufwendungEntry[]> = walter_get(
		'/api/erhaltungsaufwendungen'
	);
</script>

<WalterHeader title="Erhaltungsaufwendungen" />
<Erhaltungsaufwendungen search {rows} />
