<script lang="ts">
	import { WalterGrid, WalterUmlage, WalterHeaderNew } from '$components';
	import type { UmlageEntry } from '$types';

	const entry: Partial<UmlageEntry> = {};
	const url = `/api/umlagen`;

	const title = 'Neue Umlage';
</script>

<WalterHeaderNew {url} {entry} {title} />

<WalterGrid>
	<WalterUmlage {entry} />
</WalterGrid>
