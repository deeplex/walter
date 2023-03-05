<script lang="ts">
	import { WalterGrid, WalterUmlage, WalterHeaderNew } from '$WalterComponents';
	import type { WalterUmlageEntry } from '$WalterTypes';

	const entry: Partial<WalterUmlageEntry> = {};
	const url = `/api/umlagen`;

	const title = 'Neue Umlage';
</script>

<WalterHeaderNew {url} {entry} {title} />

<WalterGrid>
	<WalterUmlage a={entry} />
</WalterGrid>
