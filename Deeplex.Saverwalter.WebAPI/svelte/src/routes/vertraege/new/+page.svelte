<script lang="ts">
	import { WalterGrid, WalterVertrag, WalterHeaderNew } from '$components';
	import type { VertragEntry } from '$types';

	const url = `/api/vertraege`;
	const entry: Partial<VertragEntry> = {};
	const title = 'Neuer Vertrag';
</script>

<WalterHeaderNew {url} {entry} {title} />

<WalterGrid>
	<WalterVertrag {entry} />
</WalterGrid>
