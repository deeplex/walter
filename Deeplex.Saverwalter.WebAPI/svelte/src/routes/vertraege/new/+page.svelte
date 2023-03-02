<script lang="ts">
	import {
		WalterGrid,
		WalterVertrag,
		WalterHeaderNew
	} from '$WalterComponents';
	import type { WalterVertragEntry } from '$WalterTypes';

	const url = `/api/vertraege`;
	const entry: Partial<WalterVertragEntry> = {};
	const title = 'Neuer Vertrag';
</script>

<WalterHeaderNew {url} {entry} {title} />

<WalterGrid>
	<WalterVertrag {entry} />
</WalterGrid>
